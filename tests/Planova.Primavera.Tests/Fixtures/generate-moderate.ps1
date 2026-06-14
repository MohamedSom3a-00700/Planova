$path = "tests/Planova.Primavera.Tests/Fixtures/moderate.xer"
$lines = [System.Collections.ArrayList]::new()

# Header
$lines.Add('ERMHDR|7.0|moderate_project|UTF-8|3.0|6.0|6.0|6.0') | Out-Null

# Calendar
$lines.Add('%T|CALENDAR') | Out-Null
$lines.Add('%F|calendar_id|clndr_type|day_hr_cnt|week_hr_cnt|month_hr_cnt|year_hr_cnt|default_flag|name|clndr_data|suncrt_shift_cnt') | Out-Null
$lines.Add('%R|1|BASE|8.0|40.0|176.0|2112.0|Y|Standard 5-day|368:0:0:8:0:0:0:1:2:3:4:5:0:6:0:7|0') | Out-Null
$lines.Add('%R|2|BASE|8.0|40.0|176.0|2112.0|N|Night Shift|368:1:2:3:4:5:0:6:0:7:8:0|0') | Out-Null
$lines.Add('%R|3|BASE|8.0|40.0|176.0|2112.0|N|Weekend|368:0:0:8:0:0:0:6:7:1:2:3:4:5|0') | Out-Null
$lines.Add('%R|4|BASE|8.0|40.0|176.0|2112.0|N|4-10 Schedule|368:0:0:10:0:0:0:1:2:3:4:5:0:6:0:7|0') | Out-Null
$lines.Add('%R|5|BASE|8.0|40.0|176.0|2112.0|N|5-8 Schedule|368:0:0:8:0:0:0:1:2:3:4:5:0:6:0:7|0') | Out-Null
$lines.Add('%R|6|BASE|8.0|40.0|176.0|2112.0|N|6-Day Workweek|368:0:0:8:0:0:0:1:2:3:4:5:6:0:7|0') | Out-Null
$lines.Add('%R|7|BASE|8.0|40.0|176.0|2112.0|N|7-Day Workweek|368:0:0:8:0:0:0:1:2:3:4:5:6:7:0|0') | Out-Null
$lines.Add('%R|8|BASE|8.0|40.0|176.0|2112.0|N|Compressed|368:0:0:10:0:0:0:1:2:3:4:0:5:6:0:7|0') | Out-Null
$lines.Add('%R|9|BASE|8.0|40.0|176.0|2112.0|N|Custom Calendar|368:0:0:8:0:0:0:1:2:3:4:5:0:6:0:7|0') | Out-Null
$lines.Add('%R|10|BASE|8.0|40.0|176.0|2112.0|N|Shift Work|368:1:2:3:4:5:0:6:0:7:8:0|0') | Out-Null

# Project
$lines.Add('%T|PROJECT') | Out-Null
$lines.Add('%F|proj_id|proj_short_name|proj_name|plan_start_date|plan_end_date|add_date|status_code') | Out-Null
$lines.Add('%R|1|MOD|Moderate Test Project|01-Jan-2026|31-Dec-2027|01-Jan-2026|Planned') | Out-Null

# Task - 10000 activities
$lines.Add('%T|TASK') | Out-Null
$lines.Add('%F|task_id|proj_id|clndr_id|task_name|task_type|status_code|duration|start_date|end_date|phys_complete|task_code') | Out-Null

$taskTypes = @("TT_Task", "TT_Milestone", "TT_LevelOfEffort", "TT_WBS")
$statuses = @("Status_Not_Started", "Status_In_Progress", "Status_Completed")
$startDate = [DateTime]"2026-01-01"
$endDate = [DateTime]"2027-12-31"
$totalDays = ($endDate - $startDate).Days

for ($i = 1; $i -le 10000; $i++) {
    $taskType = $taskTypes[$i % $taskTypes.Length]
    $status = $statuses[$i % $statuses.Length]
    $duration = [Math]::Max(1, ($i % 200) + 1)
    $offsetDays = $i % $totalDays
    $ts = $startDate.AddDays($offsetDays)
    $te = $ts.AddDays($duration)
    $phys = [Math]::Round(($i % 100), 0)
    $lines.Add("%R|$i|1|$([Math]::Max(1, 1 + ($i % 10)))|Task $i|$taskType|$status|$duration|$($ts.ToString('dd-MMM-yyyy'))|$($te.ToString('dd-MMM-yyyy'))|$phys|M$("{0:D5}" -f $i)") | Out-Null

    if ($i % 1000 -eq 0) { Write-Host "Generated $i tasks..." }
}

# Relationships - 30000
$lines.Add('%T|TASKPRED') | Out-Null
$lines.Add('%F|task_pred_id|task_id|pred_task_id|pred_type|lag_hr_cnt') | Out-Null
$predTypes = @("FS", "SS", "FF", "SF")
$relId = 1
for ($i = 1; $i -le 10000; $i++) {
    $numPreds = [Math]::Min(4, 1 + ($i % 4))
    for ($j = 1; $j -le $numPreds -and $relId -le 30000; $j++) {
        $predTask = [Math]::Max(1, $i - ($j * 2 + ($i % 5)))
        if ($predTask -ge $i) { $predTask = $i - 1 }
        if ($predTask -lt 1) { $predTask = 1 }
        $predType = $predTypes[$relId % $predTypes.Length]
        $lag = ($relId % 40) * 8
        $lines.Add("%R|$relId|$i|$predTask|$predType|$lag") | Out-Null
        $relId++
    }
    if ($i % 1000 -eq 0) { Write-Host "Generated $relId relationships..." }
}
Write-Host "Generated $relId total relationships (capped at 30000)"

# Resource assignments - 1000
$lines.Add('%T|TASKRSRC') | Out-Null
$lines.Add('%F|rsrc_id|task_id|proj_id|target_qty|remain_qty|cost_per_qty|target_cost|act_cost') | Out-Null
for ($i = 1; $i -le 1000; $i++) {
    $taskId = 1 + ($i % 10000)
    $rsrcId = 1 + ($i % 50)
    $qty = [Math]::Max(1, $i % 500)
    $cost = [Math]::Round(($i % 100) + 10, 2)
    $lines.Add("%R|$rsrcId|$taskId|1|$qty|$qty|$cost|$([Math]::Round($qty * $cost, 2))|0") | Out-Null
}

# Resources - 50
$lines.Add('%T|RSOURCE') | Out-Null
$lines.Add('%F|rsrc_id|rsrc_name|rsrc_short_name|employee_code|rsrc_type_name|def_qty_per_hr|cost_qty_type') | Out-Null
$resourceNames = @("Engineer", "Senior Engineer", "Laborer", "Foreman", "Project Manager",
    "Site Manager", "Architect", "Structural Engineer", "Civil Engineer", "Electrical Engineer",
    "Mechanical Engineer", "Planner", "Scheduler", "Cost Engineer", "Quantity Surveyor",
    "Safety Officer", "QA Inspector", "Surveyor", "Geotechnical Engineer", "Environmental Specialist",
    "CAD Technician", "BIM Specialist", "Procurement Officer", "Logistics Coordinator", "HR Manager",
    "Accountant", "Legal Advisor", "Public Relations", "IT Support", "Administrator",
    "Secretary", "Driver", "Warehouse Keeper", "Crane Operator", "Welder",
    "Electrician", "Plumber", "Carpenter", "Steel Fixer", "Concrete Worker",
    "Painter", "Tiler", "Mason", "Rigger", "Scaffolder",
    "HVAC Technician", "Fire Safety Engineer", "Security Guard", "Cleaner", "Landscaper")
for ($i = 1; $i -le 50; $i++) {
    $lines.Add("%R|$i|$($resourceNames[$i-1])|$($resourceNames[$i-1].Substring(0,[Math]::Min(3,$resourceNames[$i-1].Length)).ToUpper())|E$( "{0:D3}" -f $i)|Labor|1.0|Ea") | Out-Null
}

# Project codes
$lines.Add('%T|PROJECTCODE') | Out-Null
$lines.Add('%F|proj_code_type_id|proj_id|code_value|proj_code_type|proj_code_name') | Out-Null
$lines.Add('%R|1|1|CIVIL|Discipline|Civil Engineering') | Out-Null
$lines.Add('%R|2|1|STRUC|Discipline|Structural Engineering') | Out-Null
$lines.Add('%R|3|1|MECH|Discipline|Mechanical Engineering') | Out-Null
$lines.Add('%R|4|1|ELEC|Discipline|Electrical Engineering') | Out-Null

# UDF type
$lines.Add('%T|UDFTYPE') | Out-Null
$lines.Add('%F|udf_type_id|table_name|udf_type_name|udf_type_label|logical_data_type') | Out-Null
$lines.Add('%R|1|TASK|Priority|Priority|TEXT') | Out-Null
$lines.Add('%R|2|TASK|Location|Location|TEXT') | Out-Null

# UDF values (just a few)
$lines.Add('%T|UDFVALUE') | Out-Null
$lines.Add('%F|udf_type_id|fk_id|udf_value_text') | Out-Null
for ($i = 1; $i -le 100; $i++) {
    $taskId = 1 + ($i % 10000)
    $lines.Add("%R|1|$taskId|$([Math]::Max(1, $i % 5))") | Out-Null
}

[System.IO.File]::WriteAllLines([System.IO.Path]::GetFullPath($path), $lines.ToArray())
Write-Host "Moderate XER file generated at $path with 10000 activities, 30000 relationships, 1000 resources"
