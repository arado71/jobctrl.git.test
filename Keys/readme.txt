Telepítési útmutató:
--------------------

- Saját self-signed certificate generálása (pl.: makecert segítségével):
	> makecert -pe -n "CN=tct.ActivityRecorder" -a sha1 -sky exchange -r -sr localmachine -ss My tct.ActivityRecorder.cer

- JobCTRL könyvtár felmásolása a megfelelő helyre (pl.: c:\JobCTRL)

- JobCTRLService.exe.config testreszabása:
	configuration\appSettings
		<add key="ScreenShotsDir" value="c:\JobCTRL\JobControlScreenShots"/> <!--- hova mentse a szerver a screenshotokat -->
		<add key="EmailsToSendDir" value="c:\JobCTRL\EmailsToSend"/> - hova mentse a szerver a kiküldendő emaileket
		<add key="DeadLetterDir" value="c:\JobCTRL\DeadLetter"/> - hova mentse a szerver a rossz üzeneteket
		<add key="VoiceRecordingsDir" value="c:\JobCTRL\VoiceRecordings"/> - hova mentse a szerver a hangfelvételeket (VoxCTRLnál érdekes csak)
		<add key="MobileStatusUpdateInterval" value="-1"/> - ha van mobil szerver akkor töröljük ki ezt a sort
		<add key="EmailSmtpHost" value="127.0.0.1"/> - emailek beállításai
		<add key="EmailSmtpPort" value="25"/>
		<add key="EmailSsl" value="false"/>
		<add key="EmailFrom" value="noreply@jobctrl.com"/>
		<add key="EmailUserName" value=""/>
		<add key="EmailPassword" value=""/>

	configuration\connectionStrings
		a 3 connection stringet állítsuk be a megfelelő sql serverre (használjuk ugyanazt a szertvert mind3 beállításnál)

	configuration\system.serviceModel\client
		állítsuk be a website api címét (ha kell használhatunk https-t de ebben az esetben ClientAPISoap bindingban a security mode="None"-ban a None-t írjuk át Transport-ra)
		állítsuk be a mobil szerver címét (ha van)

	ha esetleg nem tct.ActivityRecorder névvel hoztuk létre a self-signed cert-et:
	configuration\system.serviceModel\behaviors\serviceBehaviors\behavior\serviceCredentials
		serviceCertificate findValue értékét írjuk át a megfelelő értékre

- tűzfalon nyissuk ki a 9000-es és a 9001-es portokat

- Rendeljük hozzá a 9001-es porthoz a generált certificate-ünket. Ehhez szükség lesz a cert thumbprint-jére
  (mmc / add/remove snap-in / certificates - add - computer account - local computer - ok / personal / certificates / tct.ActivityRecorder.keys.pfx (!!) - dupla klikk / details / thumbprint):
  [127.0.0.1 és a 2148cb2a1f7b4f5ab871db5bbb0a134a221bc63d értékeket írjuk át]
	> netsh http add sslcert ipport=127.0.0.1:9001 certhash=2148cb2a1f7b4f5ab871db5bbb0a134a221bc63c appid={9d94beb0-7616-4bba-9536-b6ec1a97a30c}

- ha más userrel szeretnénk futtatni a szervert:
	írjuk át a JobCTRLServiceInstall.bat-ot és a /i paraméter után írjuk oda /account="User" /username="DOMAIN\DOMAINUSER" /password="SECRET" természetesen a megfelelő értékekkel

- ha a futtató user nem fér hozzá a certificate store-hoz, akkor adjunk neki jogot (pl.:winhttpcertcfg-vel)
	> winhttpcertcfg.exe -g -c LOCAL_MACHINE\MY -s tct.ActivityRecorder -a DOMAIN\DOMAINUSER

- ha a futtató user-nek nincs joga a beállított http namespace-ekhez, akkor adjunk neki jogot:
	> netsh http add urlacl url=http://+:8000/ user=DOMAIN\DOMAINUSER
	> netsh http add urlacl url=https://+:9001/ user=DOMAIN\DOMAINUSER

- ha a futtató user-nek nincs joga írni a saját Logs könyvtárát vagy a ScreenShotsDir, DeadLetterDir, VoiceRecordingsDir vagy EmailsToSendDir könyvtárát, adjunk neki.

- Installáljuk fel a szervert (ez rögtön el is indítja):
	> JobCTRLServiceInstall.bat

Ha nem indul el (vagyis rögtön leáll indulás után) nézzük meg az event logot. Ha az üres akkor a Logs könyvtában nézzük meg a logot.
Ha egyik helyre sem íródott semmi, akkor valószínű nincs jogunk írni a Logs könyvtárat.

Ahhoz hogy a megfelelő klienst le tudjuk generálni szükség van a website címére, és a generált kulcs publikus részére base64 formában (.cer):
mmc / add/remove snap-in / certificates - add - computer account - local computer - ok / personal / certificates / tct.ActivityRecorder - jobb klikk
all tasks / Export... - Next - No, don't export... - Base-64 emcoded X.509 (.CER) - filenév