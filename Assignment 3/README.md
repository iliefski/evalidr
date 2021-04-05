## Svar

Kodens specifikationer, utifrån testningen, ser ut att vara ungefär: "Generera två serier av mönster för följande dagar där var series mönster från samma dag måste skilja sig från den andras och där upprepningar i samma serie från en dag till nästa minimeras."

Utan att ha testkört så misstänker jag att den inte kan garantera att de två seriernas mönster för samma dag inte kolliderar. Det genererar 10 olika färger och sättet den genererar dessa, med en serie deterministiska bit shift/ xor operationer och en datum-sträng som input, gör att strängar som bara skiljer en char mycket troligen ger olika resultat men ju mer slumpartiga input-strängar desto mer närmar den sig en helt random output där risken för kollision borde vara 0.1*0.1. Input-strängarna i list1 och två genereras oberoende och adderar ett slumpartat antal minuter och ändrar mer bitar än vad som behövs vilket antagligen tar bort funktionaliteten i GenerateInteger. Det finns inget som hindrar dessa två strängarna att resultera i samma färg. Det är dock ganska liten risk att batchen av tre dagars mönster som genereras tillsammans upprepar sig. I och med att nya random minuter genereras till nästa batch ökar dock risken till överlapp mellan första och sista i grupper, det finns ett försök till att åtgärda detta men den funktionen använder inte samma seed-sträng när den ska jämföra första från aktuella med sista i föregående och fyller därmed inte sin funktion. 

GenerateInteger fungerar nog bra, trots att den har en del önödiga operationer som får den att se mer komplex ut än vad den är. För att låta den göra sitt jobb hade jag inte genererat random-minuter för var grupp om tre dagar, utan snarare bara låtit dagarna inkrementeras med resten av strängarna identiska vilket jag tror hade gett bättre resultat utifrån min antagna specifikation. 









