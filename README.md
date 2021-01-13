# DigiTek 
DigiTEK er en betegnelse på arbeidet som gjøres for digitalisering av TEK17, byggteknisk forskrift med veiledning, som eies og vedlikeholdes av Direktoratet for byggkvalitet (DiBK).
TEK17 består dels av regler og krav, dels av en veiledning som gir kvantitative og kvalitative beskrivelser, bl.a. i form av preaksepterte ytelser som et minimum for å oppfylle beskrevne krav.
Formålet med DigiTEK er å lage en plattform der byggbransjen og dens leverandører av digitaliseringsverktøy kan hente ut parametre i form av operasjonelle, kvantitative krav og preaksepterte ytelser og inkluderer dem i sine verktøy og videre inn i konkrete bygg-løsninger og BIM-modeller.

Pr januar 2021 er arbeidet i en POC-fase. Det er utarbeidet et teknisk grensesnitt, et API, som åpner for å hente ut krav og preaksepterte ytelser ut fra et sett med inputargumenter.
DigiTEK tar primært for seg kapitel 11 i TEK17: "Sikkerhet ved brann". Det er i første omgang primært de deler av forskriften og veiledningen som er på tabellform som er digitalisert. Output'en er altså på ingen måte komplett, sammenlignet med innholdet i TEK17, kapitel 11, og det kan ikke brukes ved brannprosjektering.

Selve regelsettet, sammenhengen mellom inputvariabler og output, er beskrevet i DMN (Decision Modell Notation) med bruk av Camunda sin løsning. 
Prosessering av request gjøres gjennom BPMN-modeller som også er satt opp i Camunda. Grensesnittet er json-basert.
Uttrekket inneholder:
- modelDataDictionary, som dokumenterer alle variable, in- og output, som er involvert i den konkrete request'en, inkl. detaljert URL-referanse til gjeldende TEK17 og digitalisert struktur
- modelInputs, dokumentasjon av request-variable som er forutsetning for respons'en
- modelOutputs, resultatet av kjøring av request'en. For hver digitalisert punkt fra TEK17, kap. 11, gis det 1-n antall outputvariable som beskriver fundne krav og preaksepterte ytelser.
- info
- executionInfo, bl.a. en GUID, en unik ekseveringsidentifikator.

#Disclaimer
Kun til test; løsningen er under utvikling!: API’et leverer et begrenset uttrekk av krav og preaksepterte ytelser for sikkerhet ved brann i TEK17 med veiledning. 
Unntak fra reglene er ikke tatt med, og uttrekket kan derfor ikke brukes ved prosjektering

#Non-Norwegian readers
Please contact either DiBK or Arkitektum for information.
