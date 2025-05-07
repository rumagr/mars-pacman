# Lab 01 
## Dominik Wiesendanger, Ruben Marin Grez 

## Verbesserungen zum Verhalten
- Zunächst haben wir Methoden hinzugefügt um die Geister, Pellets und Powerpellets, die Pac Man am nächsten sind zu erhalten.
- Wir haben einen Prioritätenplan erstellt, um zu entscheiden, wie Pacman handelt:
  1. Wenn Pacman in dem Zustand "powered up" ist, dann soll er Geister jagen, wenn kein Geist in der Nähe ist, soll er einfach Pellets essen.
  2. Wenn er nicht im Zustand "powered up" ist und sich mindestens ein Geist in direkter Nähe befindet (Abstand von 3 Feldern),
  dann soll er sich im Fall dass das nächste Powerpellet in einer anderen Richtung als der Geist befindet, darauf zu bewegen.
  - Falls kein Powerpellet in der Nähe ist, soll Pacman in die entgegengesetzte Richtung vom Geist fliehen.
  - Sollte die direkt entgegengesetzte Richtung nicht begehbar sein, wird die am weitesten entfernte Position vom nächsten Geist als Ziel gesetzt.
  3. Sollten sich 2 oder mehr Geister in Sichtweite (8 Felder) befinden und ein Powerpellet in der Nähe ist, soll er dies essen
  4. Wir haben einen counter hinzugefügt, welcher bis 15 Pellets hoch zählt. Sollte Pacman diese Anzahl erreicht haben, wird es Zeit ein Powerpellet zu essen, ansonsten werden normale Pellets gegessen.
     - Hier haben wir darauf geachtet, dass Pacman sich nicht diagonal bewegt, da es dann zu einzelnen Pellets kommt, für die man später einen Umweg einschlagen müsste.
  5. Falls keine der Verhaltensweisen möglich ist, bewegt Pacman sich random.