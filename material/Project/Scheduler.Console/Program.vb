Imports System
Imports System.Collections.Generic
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
            If args Is Nothing OrElse args.Length = 0 Then Return 2
            Dim target = args(0)
            Dim ctx As New WorkflowContext()
            Dim wf As New WorkflowDefinition With {.Name = target & "E2E"}

            Select Case target
                Case "RouteDispatch"
                    wf.Blocks.Add(New RouteDispatchBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", Env("E2E_DB_ROOT", "runtime_db")},
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
                            {"dbRoot", Env("E2E_DB_ROOT", "runtime_db")},
                            {"lotId", Env("E2E_LOT_ID", "LOT-001")},
                            {"reason", Env("E2E_HOLD_REASON", "E2E_HOLD")}
                        }
                    })
                Case "EquipmentCheck"
                    wf.Blocks.Add(New EquipmentCheckBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", Env("E2E_DB_ROOT", "runtime_db")},
                            {"eqId", Env("E2E_EQ_ID", "EQ-01")},
                            {"requiredMode", Env("E2E_REQUIRED_MODE", "")}
                        }
                    })
                Case "AlarmNotify"
                    wf.Blocks.Add(New AlarmNotifyBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"url", Env("E2E_ALARM_URL", "http://mock/alarm")},
                            {"message", Env("E2E_MESSAGE", "E2E")}
                        }
                    })
                Case "TokenGenerate_CommandSubmit"
                    ' Dependency workflow: these two blocks are one E2E target and must run together.
                    wf.Blocks.Add(New TokenGenerateBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", Env("E2E_DB_ROOT", "runtime_db")},
                            {"correlationId", Env("E2E_CORRELATION_ID", "CMD-001")}
                        }
                    })
                    wf.Blocks.Add(New CommandSubmitBlock With {
                        .Parameters = New Dictionary(Of String, Object) From {
                            {"dbRoot", Env("E2E_DB_ROOT", "runtime_db")},
                            {"command", Env("E2E_COMMAND", "START")},
                            {"mqTopic", Env("E2E_MQ_TOPIC", "command.submit")}
                        }
                    })
                Case Else
                    Return 3
            End Select

            LastContext = ctx
            Dim runner As New WorkflowRunner()
            runner.Run(wf, ctx)
            Return 0
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
