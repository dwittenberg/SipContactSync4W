#
# csv2csv - Script sample to convert CSV-Fields 
#
# 20140131 Version 1.0 InitialVersion 
#
# Pending: Errorhandling, Eventlog 	

Param (
	[string]$configxml = ".\csv2csv.xml" #KonfigXML
)

#region Global preferences
cls
write-host "csv2csv:------------------ Start ----------------------"
Set-PSDebug -strict
#$ErrorActionPreference = "SilentlyContinue"
#$ErrorActionPreference = "Continue"
$ErrorActionPreference = "Stop"
$error.clear()
$starttimetotal=get-date
#endregion

#region Functions to get called multiple times

# Check für $error and end script with eventlog and message
function exitonerror  {
	param (
		[int]$eventID = 0 ,
		[string]$message = "no message given", 
		[boolean]$always =$false  # allow to force exit
	)
	if ($error -or $always) {
		write-eventlog `
			-entrytype Error `
			-logname Application `
			-category 0 `
			-eventID $eventID `
			-Source 'csv2csv' `
			-Message $message
		write-error "ExitOnError: EventID=$eventid  Message=$message"
		stop-transcript
		exit $eventid
	}
}

# Check für $error and log warning but clear error an continue
function warnonerror  {
	param (
		[int]$eventID = 0 ,
		[string]$message = "no message given"
	)
	if ($error) {
		write-Warning ("WarnOnError: EventID=$eventid  Message=$message")
		write-eventlog `
			-entrytype Warning `
			-logname Application `
			-category 0 `
			-eventID $eventID `
			-Source 'csv2csv' `
		Â 	-Message $message
		$error.clear()
	}
}

# convert HEX-Characters in CSV-Exports
function hex2string {
	param($hexstring)
	
	if ($hexstring -eq $null) { 
		# ignore null string
	}
	elseif ($hexstring -eq "X'00'") { 
		#  HEX in LDIF-File is Null
		write-host ("csv2csv:Updateproperty:hex2string:Result=NULL")
		$hexstring=$null
	}
	elseif ($hexstring.startswith("X'")) {
		# Convert HEX in LDIF-File to String
		$result=""
		
		for ($i=2;$i -le ($hexstring.length  - 3); $i=$i+2) {
			write-host ("csv2csv:hex2char: Parsing "+$i+"/"+$hexstring.length+":"+ ($hexstring).substring($i,2))
			$result = $result + [char][Convert]::ToInt32($hexstring.substring($i,2), 16)
		}
		write-host ("csv2csv:Updateproperty:hex2string:Result="+$result)
		$result
	}
}


# Update AD Properties and handle delete of properties
function Update-property {
	
	param (
		[ref]$updatecontact,  #ADSI Property
		[string]$updateproperty,
		[string]$updatevalue
	)
	
	#$ADS_PROPERTY_UPDATE = 2
	$ADS_PROPERTY_CLEAR = 1
	
	#write-host "UpdateProperty:Object  :"$updatecontact
	#write-host "UpdateProperty:Property:"$updateproperty
	
	
	if (($updatevalue -eq "") -or ($updatevalue -eq $null)) {
		write-host ("csv2csv:Updateproperty: $updateproperty = CLEAR")
		$updatecontact.value.PutEx($ADS_PROPERTY_CLEAR, $updateproperty, 0)
		#$updatecontact.$updateproperty.clear()
	}
	else {
		write-host ("csv2csv:updateproperty: $updateproperty = $updatevalue")
		$updatecontact.value.Put($updateproperty,[string]$updatevalue)

		#$updatecontact.$updateproperty = ([string]$updatevalue)
	}
	#$entry.PutEx($ADS_PROPERTY_UPDATE, â€œKeywordsâ€, $keywordsArray)
}

# generate valid unique CN für creations and matching
function create-targetcn {
	# Function to create a DN based on the source data
	# used to build CN and hashtable to find existing contact
	# Result must be:
	# - unique within that OU
	# - valid characters für a CN
	# - Reproducable für later matching during Updates
	param (
		$sourcerecord
	)
	[string]$primarykeyvalue=([string]$sourcerecord.mail).tolower()
	if ($primarykeyvalue.LastIndexOf("<") -lt $primarykeyvalue.LastIndexOf(">")) {
		write-host ("csv2csv::Create-TargetCN:Create-TargetCN MAIL contains < >. Using inner part")
		$primarykeyvalue= $primarykeyvalue.Substring($primarykeyvalue.LastIndexOf("<")+1)
		$primarykeyvalue= $primarykeyvalue.Remove($primarykeyvalue.LastIndexOf(">"))
	}
	$primarykeyvalue = ($primarykeyvalue  -replace "[^a-zA-Z\d\.@\-]")
	exitonerror 1 ("csv2csv:Create-TargetCN:Unhandled Error CreateTargetDN:"+$error)
	write-host ("csv2csv:Create-TargetCN: Key=$primarykeyvalue")
	$primarykeyvalue
}
#endregion Functions

#region Eventlog Definition and initialization
#Source bei Bedarf definieren
#if (![System.Diagnostics.EventLog]::SourceExists("csv2csv")) {
#    new-eventlog -logname 'Application' -Source 'csv2csv'
#}

# Eventlogeintrag erzeugen
#write-eventlog `
#-entrytype Information `
# -logname Application `
# -eventID 1 `
# -category 0 `
# -Source 'csv2csv' `
# -Message "Start mit Parameter $configxml"
#endregion

#region Loading ConfigXML
write-host "csv2csv:ConfigXML:------------------ Loading Config XML ----------------------"
[string]$basedir=(split-path $SCRIPT:MyInvocation.MyCommand.Path -parent)
if (($configxml) -eq "") {
	if (test-path -path ($basedir+"\config\"+($env:COMPUTERNAME)+".xml") -pathtype leaf) {
		$configxml = ($basedir+"\config\"+($env:COMPUTERNAME)+".xml")
		write-host "csv2csv:ConfigXML:Using Config (PCName): $configxml"
	}
	elseif (test-path -path ($basedir+"\config\sync1.xml") -pathtype leaf) {
		$configxml = ($basedir+"\config\sync1.xml")
		write-host "csv2csv:ConfigXML:Using Config (Default): $configxml"
	}
	else {
		write-host "csv2csv:ConfigXML:Unable to find Config File"
	}
}
else {
	write-host "csv2csv:ConfigXML:Using Config (Params): $configxml"
}
write-host "csv2csv:ConfigXML:Loading ConfigXML from $configxml"
try {
	$config=([xml](get-content -path $configxml)).config
}
catch {
	write-host ("csv2csv:ConfigXML: Failed loading" +$error)
	exitonerror -eventid 2 -message ("csv2csv:ConfigXML: unable to Load:" + $error)
}
$oldwindowstitle = $host.ui.RawUI.WindowTitle
$host.ui.RawUI.WindowTitle = "csv2csv Running: $configxml"

#endregion

#region Transcript of Actions to Logfile and purge older logs
$transcriptfile = $config.logpath+"csv2csv."+(get-date -format yyyyMMddHHmmss)+".log"
write-host "csv2csv:Logging:Transcript to:$transcriptfile"
try {stop-transcript} catch {$error.clear()}
start-transcript -append -path $transcriptfile 
$error.clear()  # remove error, if transcript was already running

write-host ("csv2csv:Logging:Logfilepath :"+$config.logpath)
write-host ("csv2csv:Logging:LogFileAge  :"+$config.logpurgeage)

write-host ("Purging Logfiles older than "+$config.logpurgeage+" days  from "+$config.logpath)
foreach ($file in (get-childitem $config.logpath | where {$_.lastwritetime -lt ( (get-date).adddays(-$config.logpurgeage))} )) {
	write-host ("Purging old log("+$config.logpurgeage+"):" + $config.logpath +$file )
	remove-item -path ($config.logpath +$file)
}
#endregion


$mode=$config.import.filename

#region ------------------------- Load SourceCSV ------------------------- 
write-host ("csv2csv:Import:Filename :"+ $config.import.filename)
if ($config.import.delimiter -eq "") {$config.import.delimiter = ","}
write-host ("csv2csv:Import:Delimiter:"+ $config.import.delimiter)
if ($config.import.encoding -eq "") {$config.import.encoding = "default"}
write-host ("csv2csv:Import:Encoding :"+ $config.import.encoding)
if ($config.import.header -eq "") {
	write-host ("csv2csv:Import:Header   :Default")
	$csvdata = import-csv `
	   -path $config.import.filename `
	   -delimiter $config.import.delimiter `
	   -encoding $config.import.encoding
}
else {
	write-host ("csv2csv:Import:Header   :"+ $config.import.header)
	$csvindata = import-csv `
	   -path $config.import.filename `
	   -delimiter $config.import.delimiter `
	   -encoding $config.import.encoding `
	   -header  $config.import.header
}
write-host ("csv2csv:Import:Total Records loaded  :"+ $csvdata.count)
#endregion ------------------------- Import ------------------------- 


#region ------------------------- Translate ------------------------- 
# use the Header and Properties to modify the fields. 

[array]$csvoutdata=@()
[array]$propertylist = $config.translate | gm -MemberType property | %{$_.name}
write-host ("csv2csv:Translate:Header:"+ ($propertylist -join","))

for ($count=0; $count -lt $csvdata.count; $count++){
	$source = $csvdata[$count]
	write-progress `
		-Activity "Data Transformation" `
		-Status  ("Line "+$count+"/"+$csvdata.count+": "+$source.dn)`
		-PercentComplete ($count/$csvdata.count*100)
	if ($config.debuglevel -gt 1 ){	write-host ("csv2csv:Processing "+$count+"/"+$csvdata.count+":"+ $source.dn)}

	$newline = ("" | select $propertylist)   # preparing target object
	foreach ($property in $propertylist) {
		$newline.$property =  invoke-expression ($config.translate.($property).tostring())
	}	  
	$csvoutdata += $newline
}
write-host ("csv2csv:Translate:Done")	

#endregion ------------------------- Translate ------------------------- 

#region -------------------------  Export CSV ------------------------- 


write-host ("csv2csv:Export:Filename :"+ $config.export.filename)
if ($config.export.delimiter -eq "") {$config.export.delimiter = ","}
write-host ("csv2csv:Export:Delimiter:"+ $config.export.delimiter)
if ($config.export.encoding -eq "") {$config.export.encoding = "default"}
write-host ("csv2csv:Export:Encoding :"+ $config.export.encoding)

write-host "csv2csv:Start Export"
write-host ("csv2csv:Export:Header   : Default")
$csvoutdata | export-csv `
   -path $config.export.filename `
   -delimiter $config.export.delimiter `
   -encoding $config.export.encoding `
   -force `
   -NoTypeInformation `

#endregion ------------------------- Export CSV ------------------------- 


write-host ("csv2csv:Summary:TotalTime (Sec):"+((Get-Date) -$starttimetotal).totalseconds)

# Eventlogeintrag erzeugen
#write-eventlog `
# -entrytype Information `
#Â -logname Application `
#Â -category 0 `
#Â -eventID 2 `
#Â -Source 'csv2csv' `
#Â -Message "End mit Parameter $configxml `n`r $summary"

$host.ui.RawUI.WindowTitle = $oldwindowstitle

write-host "csv2csv:END ---------------------------------------------------------"
stop-transcript