function Draw-Hierarchy{
param(
    [System.Collections.ArrayList]$Array
)

$Array.Reverse()

for($i=0;$i -lt $Array.Count;$i++){

    if(
        $Array[$i+1] -and 
        $Array[$i].Hierarchy.IndexOf('|_') -lt $Array[$i+1].Hierarchy.IndexOf('|_')
    ){
    $z=$i+1
    $ind=$Array[$i].Hierarchy.IndexOf('|_')
        while($Array[$z].Hierarchy[$ind] -ne '|'){
            $string=($Array[$z].Hierarchy).ToCharArray()
            $string[$ind]='|'
            $string=$string -join ''
            $Array[$z].Hierarchy=$string
            $z++
            if($Array[$z].Hierarchy[$ind] -eq '|'){break}
            }
        }
    }

$Array.Reverse()
return $Array

}