Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports FileDb
Imports Workflow
Imports Engine
Imports RouteDispatch
Imports HoldLot
Imports EquipmentCheck
Imports AlarmNotify
Imports TokenGenerate
Imports CommandSubmit

Namespace SchedulerApp
    Public Module Program
        Public LastContext As WorkflowContext

        Public Function Main(args As String()) As Integer
            Dim dbRoot = Env("E2E_DB_ROOT", "runtime_db")
            Dim instruction = LoadReadyInstruction(dbRoot, args)
            If instruction Is Nothing Then Return 2

            Dim target = instruction("TARGET")
            Dim ctx As New WorkflowContext()
            Dim wf As New WorkflowDefinition With {.Name = target & "E2E"}
            ctx.SetValue("E2E.Target", target)
            ctx.SetValue("E2E.SOP", instruction("SOP"))

            Select Case target
                Case "RouteDispatch"
                    wf.Blocks.Add(New RouteDispatchBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"paramDbRoot", Env("E2E_PARAM_DB_ROOT", dbRoot)},
                            {"fab", Env("E2E_FAB", "FAB1")},
                            {"lotId", Env("E2E_LOT_ID", "LOT-001")},
                            {"product", Env("E2E_PRODUCT", "")},
                            {"conditionMode", Env("E2E_CONDITION_MODE", "AUTO")},
                            {"allowSubRoute", EnvBool("E2E_ALLOW_SUB_ROUTE", True)},
                            {"holdIfNoRecipe", EnvBool("E2E_HOLD_IF_NO_RECIPE", True)},
                            {"maxQueue", EnvInt("E2E_MAX_QUEUE", 10)}
                        }
                    })
                Case "HoldLot"
                    wf.Blocks.Add(New HoldLotBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"paramDbRoot", Env("E2E_PARAM_DB_ROOT", dbRoot)},
                            {"lotId", Env("E2E_LOT_ID", "LOT-001")},
                            {"reason", Env("E2E_HOLD_REASON", "E2E_HOLD")},
                            {"fab", Env("E2E_FAB", "FAB1")},
                            {"product", Env("E2E_PRODUCT", "")}
                        }
                    })
                Case "EquipmentCheck"
                    wf.Blocks.Add(New EquipmentCheckBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"paramDbRoot", Env("E2E_PARAM_DB_ROOT", dbRoot)},
                            {"eqId", Env("E2E_EQ_ID", "EQ-01")},
                            {"requiredMode", Env("E2E_REQUIRED_MODE", "")}
                        }
                    })
                Case "AlarmNotify"
                    wf.Blocks.Add(New AlarmNotifyBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"configDbRoot", Env("E2E_CONFIG_DB_ROOT", dbRoot)},
                            {"channel", Env("E2E_ALARM_CHANNEL", "DEFAULT")},
                            {"url", Env("E2E_ALARM_URL", "http://mock/alarm")},
                            {"message", Env("E2E_MESSAGE", "E2E")}
                        }
                    })
                Case "TokenGenerate_CommandSubmit"
                    ' Dependency workflow: these two blocks are one E2E target and must run together.
                    wf.Blocks.Add(New TokenGenerateBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"paramDbRoot", Env("E2E_PARAM_DB_ROOT", dbRoot)},
                            {"correlationId", Env("E2E_CORRELATION_ID", "CMD-001")}
                        }
                    })
                    wf.Blocks.Add(New CommandSubmitBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", dbRoot},
                            {"paramDbRoot", Env("E2E_PARAM_DB_ROOT", dbRoot)},
                            {"masterDbRoot", Env("E2E_MASTER_DB_ROOT", dbRoot)},
                            {"command", Env("E2E_COMMAND", "START")},
                            {"mqTopic", Env("E2E_MQ_TOPIC", "command.submit")},
                            {"mqEndpoint", Env("E2E_MQ_ENDPOINT", "COMMAND_SUBMIT")}
                        }
                    })
                Case Else
                    Return 3
            End Select

            LastContext = ctx
            UpdateInstructionStatus(dbRoot, instruction, "RUNNING")
            Dim runner As New WorkflowRunner()
            runner.Run(wf, ctx)
            UpdateInstructionStatus(dbRoot, instruction, "DONE")
            Return 0
        End Function

        Private Function LoadReadyInstruction(
            dbRoot As String,
            args As String()) As Dictionary(Of String, String)

            Dim requestedTarget As String = Nothing
            If args IsNot Nothing AndAlso args.Length > 0 Then requestedTarget = args(0)

            Dim db As New FileDbSession(dbRoot)
            Return db.Query(
                    "E2E_SOP_CONTROL",
                    Function(r)
                        Return V(r, "STATUS") = "READY" AndAlso
                            (String.IsNullOrWhiteSpace(requestedTarget) OrElse
                             String.Equals(V(r, "TARGET"), requestedTarget, StringComparison.OrdinalIgnoreCase))
                    End Function,
                    "SELECT TOP 1 TARGET,SOP FROM E2E_SOP_CONTROL WHERE STATUS='READY'").
                OrderBy(Function(r) V(r, "TARGET")).
                ThenBy(Function(r) V(r, "SOP")).
                FirstOrDefault()
        End Function

        Private Sub UpdateInstructionStatus(
            dbRoot As String,
            instruction As IDictionary(Of String, String),
            status As String)

            Dim db As New FileDbSession(dbRoot)
            db.Update(
                "E2E_SOP_CONTROL",
                Function(r)
                    Return V(r, "TARGET") = instruction("TARGET") AndAlso
                        V(r, "SOP") = instruction("SOP")
                End Function,
                Sub(r) r("STATUS") = status,
                "UPDATE E2E_SOP_CONTROL SET STATUS='" & status & "' WHERE TARGET=@TARGET AND SOP=@SOP")
        End Sub

        Private Function V(row As IDictionary(Of String, String), name As String) As String
            Dim value As String = Nothing
            If row IsNot Nothing AndAlso row.TryGetValue(name, value) Then Return value
            Return ""
        End Function

        Private Function Env(name As String, defaultValue As String) As String
            Dim value = Environment.GetEnvironmentVariable(name)
            If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Return value
        End Function

        Private Function EnvBool(name As String, defaultValue As Boolean) As Boolean
            Dim value = Environment.GetEnvironmentVariable(name)
            If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Return String.Equals(value, "Y", StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(value, "TRUE", StringComparison.OrdinalIgnoreCase) OrElse value = "1"
        End Function

        Private Function EnvInt(name As String, defaultValue As Integer) As Integer
            Dim value = Environment.GetEnvironmentVariable(name)
            Dim parsed As Integer
            If Integer.TryParse(value, parsed) Then Return parsed
            Return defaultValue
        End Function
    End Module
End Namespace
