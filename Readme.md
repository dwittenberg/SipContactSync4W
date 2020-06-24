# Beschreibung
Dieses Programm synchronisiert das Telefonbuch von Phoner mit einer CSV auf einem beliebigen Pfad. Vorzugsweise Onedrive (o.ä.) oder ein Netzwerklaufwerk. 

Achtung: PhonerLite speichert seine CSV nur, wenn es geschlossen wird und ließt die
CSV wenn es geöffnet wird. Daher bitte PhonerLite schließen, bevor der Sync ausgeführt wird.

## Ablauf:
1. Extern und Intern werden geladen.
2. Suche nach markierten Änderungen auf Extern und Übertragung dieser auf Intern.
3. Vergleich von Intern mit Extern. Änderungen werden in Extern gespeichert.
4. Speichern der neuen CSV.

## Sonderfälle - Programmverhalten
### Konflikte
Wird an 2 Geräten eine Änderung am Eintrag vorgenommen, bleibt der Eintrag erhalten, der zuerst Synchronisiert wird. Der andere Nutzer muss seine Änderung erneu machen, nachdem die Änderung des Anderen geladen wurde.

### Neuer PC
Die Datenbank am Netzwerklaufwerk wird gelesen. Danach wird das eigene Gerät der Geräteliste hinzugefügt und der Status für jeden Eintrag intern auf Neu gesetzt. Nachdem alles gelesen wurde, steht der Status auf UpToDate.

## Allgemeines Programmverhalten
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
