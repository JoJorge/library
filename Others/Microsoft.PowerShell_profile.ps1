
# Load posh-git example profile, which will setup a prompt
#. 'C:\Users\JoJorge\Documents\WindowsPowerShell\Modules\posh-git\profile.example.ps1'
$poshGitModule = Get-Module posh-git -ListAvailable | Sort-Object Version -Descending | Select-Object -First 1
if ($poshGitModule) {
    $poshGitModule | Import-Module
}
elseif (Test-Path -LiteralPath ($modulePath = Join-Path (Split-Path $MyInvocation.MyCommand.Path -Parent) (Join-Path src 'posh-git.psd1'))) {
    Import-Module $modulePath
}
else {
    throw "Failed to import posh-git."
}

#& 'C:\Program Files\Git\usr\bin\ssh-add.exe'