$ErrorActionPreference = "Stop"
$dll = $args[0]
$bin = Split-Path -Parent $dll

[AppDomain]::CurrentDomain.add_AssemblyResolve({
    param($sender, $eventArgs)
    $name = (New-Object Reflection.AssemblyName($eventArgs.Name)).Name + ".dll"
    $candidate = Join-Path $bin $name
    if (Test-Path $candidate) {
        return [Reflection.Assembly]::LoadFrom($candidate)
    }
    return $null
})

function Has-Attribute($member, [string] $attributeName) {
    foreach ($attr in $member.GetCustomAttributes($false)) {
        if ($attr.GetType().FullName -eq $attributeName) {
            return $true
        }
    }
    return $false
}

$testClass = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute"
$testMethod = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"
$testInitialize = "Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute"
$testCleanup = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute"

$assembly = [Reflection.Assembly]::LoadFrom($dll)
$tests = 0
$failed = 0

foreach ($type in $assembly.GetTypes()) {
    if (-not (Has-Attribute $type $testClass)) {
        continue
    }

    $initializers = @($type.GetMethods() | Where-Object { Has-Attribute $_ $testInitialize })
    $cleanups = @($type.GetMethods() | Where-Object { Has-Attribute $_ $testCleanup })

    foreach ($method in $type.GetMethods()) {
        if (-not (Has-Attribute $method $testMethod)) {
            continue
        }

        $tests += 1
        $instance = [Activator]::CreateInstance($type)
        $name = $type.FullName + "." + $method.Name

        try {
            foreach ($init in $initializers) {
                [void] $init.Invoke($instance, @())
            }
            [void] $method.Invoke($instance, @())
            Write-Output "PASS $name"
        } catch [Reflection.TargetInvocationException] {
            $failed += 1
            Write-Output "FAIL $name"
            Write-Output $_.Exception.InnerException.ToString()
        } catch {
            $failed += 1
            Write-Output "FAIL $name"
            Write-Output $_.Exception.ToString()
        } finally {
            foreach ($cleanup in $cleanups) {
                try {
                    [void] $cleanup.Invoke($instance, @())
                } catch {
                    $failed += 1
                    Write-Output "FAIL cleanup $name"
                    Write-Output $_.Exception.ToString()
                }
            }
        }
    }
}

Write-Output "Total tests: $tests"
Write-Output "Failed tests: $failed"

if ($tests -eq 0) {
    exit 3
}
if ($failed -ne 0) {
    exit 1
}
exit 0
