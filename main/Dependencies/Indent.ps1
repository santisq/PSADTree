function Indent{
param(
    [String]$String,
    [Int]$Indent
)

$x='_';$y='|';$z='    '

switch($Indent){
    {$_ -eq 0}{return $String}
    {$_ -gt 0}{return "$($z*$_)$y$x $string"}    
    }
}