# Status client - Specifikace požadavků
Filip Komárek  
verze 0.3  
2023/02/20

## Úvod

## Účel dokumentu
Tento dokument popisuje účel, způsob fungování a základní požadavky tohoto software.

## Pro koho je dokument určený
Pro vývojáře a uživatele software

## Kontakty
Email: filip@fkomarek.eu  
Telegram: filip2cz   
Discord: Filip2cz#9069  
Mastodon: @filip2cz@mastodon.arch-linux.cz  
Instagram: @filip2czprivate  
SINET: hyper -> 9385 -> 2000

## Produkt jako celek
Tento program bude klientem pro https://github.com/BotoX/ServerStatus. Klient vytvářím z důvodu zastaralosti klientů (klient je psán ve staré verzi python2), kvůli čemuž se špatně rozjíždí na Windows (je třeba ručně stáhnout některé knihovny atd.).

## Funkce
Program se připojí na otevřený port na serveru, autentifikuje se uživatelským jménem a heslem a poté bude posílat pravidelně data ohledně využití zdrojů počítače.

## Uživatelské skupiny
Serveroví administrátoři

## Provozní prostředí
Na serveru

## Uživatelské prostředí
Žádné nebude, o uživatelské prostředí se stará operační systém v rámci jeho systému služeb.

## Omezení návrhu a implementace
Software bude vyvíjen primárně pro systémy Microsoft Windows, respektive pro Windows 10 a novější, a také pro Windows server 2019 a novější. Měl by ale potenciálně fungovat i na starších verzích.

## Předpoklady a závislosti
Program bude určen pro Windows, protože na Linuxové a Unixové systémy již existují funkční klienti. Software bude v C# v .NET, takže je nutné mít v systému všechny závislosti pro tento jazyk a framework.

## Vlastnosti systému
- autentizace pomocí uživatelského jména a klíče
- pravidelné odesílání informací o systému (vytížení procesuru, využití RAM, ...)

## Nefunkční požadavky
- běžící server https://github.com/BotoX/ServerStatus nebo jeho upravená verze, jenž bude stále podporovat tento klient
- server nepodporuje šifrování, takže nebude implementováno ani v klientovi, vzhledem k tomu že není potřeba - všechna odesílaná data jsou viděna v otevřeném webovém rozhraní
- pokud připojení spadne, otevře se během pár vteřin nové, kde znovu proběhne autentizace a poté se bude pokračovat v odesílání dat
