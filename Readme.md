# Beschreibung
Dieses Programm synchronisiert das Telefonbuch von Phoner mit einer CSV auf einem beliebigen Pfad. Vorzugsweise Onedrive (o.ä.) oder ein Netzwerklaufwerk. Der Sync wird momentan vom Nutzer händisch angestoßen.

Achtung: PhonerLite speichert seine CSV nur, wenn es geschlossen wird und ließt die
CSV wenn es geöffnet wird. Daher bitte PhonerLite schließen, bevor der Sync ausgeführt wird.
## Hinweis: Diese Software ist im Beta Status. Über Rückmeldungen freue ich mich! ;-) 
## Ablauf:
1. Die Konfiguration wird geladen.
2. Extern und Intern werden geladen.
3. Suche nach markierten Änderungen auf Extern und Übertragung dieser auf Intern.
4. Vergleich von Intern mit Extern. Änderungen werden in Extern gespeichert.
5. Speichern der neuen CSV.

## Sonderfälle - Programmverhalten
### Konflikte
Wird an 2 Geräten eine Änderung am Eintrag vorgenommen, bleibt der Eintrag erhalten, der zuerst Synchronisiert wird. Der andere Nutzer muss seine Änderung erneu machen, nachdem die Änderung des Anderen geladen wurde.

### Neuer PC
Die Datenbank am Netzwerklaufwerk wird gelesen. Danach wird das eigene Gerät der Geräteliste hinzugefügt und der Status für jeden Eintrag intern auf Neu gesetzt. Nachdem alles gelesen wurde, steht der Status auf UpToDate.

### Installation
Ich bin kein Premium Dev - daher habe ich kein zertiziziertes Zertifikat. Dpwnload unter: https://ju-da.space/setup/PhonerLiteSync429/index.html
Daher muss mein Zertifikat einmalig auf dem **Localen Computer** unter **Vertrauenswürdige Stammzertifikate** gespichert werden. Es ist unter "Additional Links" als "Publisher Certificate" zu finden. **Sonnst schlägt die Installation fehl!**

### Update
Nach einem Update muss das Programm einmal von Hand geöffnet und der Sync angestoßen werden. Sonnst startet Phoner weiterhin die alte Version.

## Allgemeines Programmverhalten
### Konfiguraion
Die Konfiguration wird als JSON unter AppData abgelegt. Der vollständige Pfad ist: %AppData%\PhonerLiteContactSync\Settings.json

Ist keine vorhanden werden Standardwerte angezeigt die bearbeitet werden können. Beim Ausführen des Sync werden die Settings gespeichert.

### Neuer Eintrag:
Der erstellende PC fügt den Eintrag in zu Extern hinzu und markiert seinen Status mit +. Wenn alle Geräte das übernommen haben wird das + entfernt.

### Ändern:
Um ändern zu können muss man die letzte Änderung schon gelesen und nach Local Übertraben haben.

Der ändernde PC markiert seinen Status mit *. Ein anderer PC kann eine Bearbeitung hochladen, wenn er die letzte Änderung schon vorher gelesen und nach Local übertragen hat.

### Löschen:
Der löschende PC hängt ein - an den eigenen Status. Jeder, der gelöscht hat, macht auch ein -. Wenn alle ein - haben, wird der Eintrag entfernt. Vom Netzwerklaufwerk entfernt.

## Technische Doku:
### Aufbau der .csv im Sync-Ordner

1 #; [PC1Name];[PC2Name]
2 [Tel];[Name];;[Kommentar];[NR][PC1Status];[NR][PC2Status]

### Status: 
    ""           :  neu für diesen PC
    [DateTime]+:  Neu
    [DateTime] :  UpToDate
    [DateTime]*:  Bearbeitet
    [DateTime]-:  Gelöscht

### PCName: 
Der Name des PC in den Systemeinstellungen. Könnte auch zufällig generiert werden und irgendwo gespeichert sein.

### DateTime: "yyyy-MM-dd HH:mm:ss,fff"
