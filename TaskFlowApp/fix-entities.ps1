$content = Get-Content 'd:\Bhavya_Raval\TaskManager\task-manager-ui\src\pages\Dashboard.jsx' -Raw
$content = $content -replace '&#39;', "'"
$content = $content -replace '&quot;', '"'
$content = $content -replace '&lt;', '<'
$content = $content -replace '&gt;', '>'
Set-Content 'd:\Bhavya_Raval\TaskManager\task-manager-ui\src\pages\Dashboard.jsx' -Value $content -Encoding UTF8