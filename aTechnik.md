Aufbau der .csv im Sync-Ordner

1 # [PC1Name];[PC2Name]
2 [Tel];[Name];;[Kommentar];[PC1Status];[PC2Status]

Status: 
    ""           :  neu für diesen PC
    [DateTime]*:  neu vom Ersteller
    [DateTime] :  lastRead
    [DateTime]+:  lastUpdate
    [DateTime]-:  delete

PCName: 
Der Name des PC in den Systemeinstellungen. Könnte auch zufällig generiert werden und irgendwo gespeichert sein.

DateTime: "yyyy-MM-dd HH:mm:ss,fff"

Ablauf:
Nach dem Lesen schreibt man das Datum in seine Spalte. Es wird verglichen, ob es ein Update gibt, das neuer als das eigene Letzte lesen ist. Wenn ja wird es übernommen.
Ist etwas als gelöscht markiert, wird es erst entfernt, wenn alle das Element bei sich gelöscht haben.

Konflikte:
Wer zuerst ändert wird behalten. Weitere Änderungen sind erst möglich, wenn man die andere Änderung zu sich übertragen hat.


Neu:
Der erstellende PC markiert seine Änderung mit +. Wenn alle Geräte das übernommen haben wird das + entfernt. Ändern ist möglich, wenn man es schon gelesen hat.

Ändern:
Der ändernde PC markiert seinen Eintrag mit *. Ein anderer PC kann bearbeiten, wenn er es schon vorher gelesen hat.

Löschen:
Der löschende PC hängt ein - an. Jeder, der gelöscht hat, macht auch ein -. Wenn alle ein - haben, wird der Eintrag entfernt.