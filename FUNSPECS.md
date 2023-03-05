# Funkční specifikace

- verze: 0.4
- datum: 5. března 2023
- autor: Filip Komárek

## O tomto dokumentu
Účel tohoto dokumentu je specifikovat funkční požadavky.  
Dokument je určen pro uživatele, kteří chtějí lépe pochopit fungování tohoto programu.

## Použití
Software bude sloužit jako klient k softwaru Status od BotoX (https://github.com/BotoX/ServerStatus). Software bude neustále spuštěn na zařízení a bude pravidelně každou vteřinu odesílat data serveru, který je pak bude zobrazovat na webové stránce (příklad serveru: https://status.fkomarek.eu). Software budou pouze odesílat data serveru, nebude nijak zobrazovat uživateli cokoliv.

## Architektura software
Po spuštění si software z konfiguračního souboru uloženého ve složce s programem souboru získá uživatele a heslo pro autentizaci a poté je odešle serveru pro ověření identity, jehož adresu také získá z konfiguračního souboru. Odesílaní data jsou ve formátu `uživatel:heslo`  
  
Poté bude každou vteřinu získávat data o aktuálním stavu systému a odesílat je serveru. Odesílaná data jsou ve formátu:  
`update {"online6": bool,  "uptime": int, "load": double, "memory_total": int, "memory_used": int, "swap_total": int, "swap_used": int, "hdd_total": int, "hdd_used": int, "cpu": double, "network_rx": int, "network_tx": int }`, kde  `bool`, `int` a `double` označují typ proměnné, jenž bude odesílána. 
  
Uživatel na zařízení neuvidí cokoliv z programu. Program bude určen ke spuštění jako service systému nebo pomocí scheduleru (plánovače procesů). Pokud uživatel spustí program ručně, tak neuvidí žádný output, pokud nebude v konfiguračním souboru nastavená proměnná `debug` na `true`. Pokud bude nastaveno že `debug` se rovná `true`, tak uživatel uvidí aktuálně odesílaná data ve formátu, v jakém jsou odesílána na server (kromě autentizace).  
  
Pokud program ztratí spojení se serverem, bude spojení obnoveno a bude znovu provedena autentizace. Pokud bude program při autentizaci odmítnut serverem, nebo se spojení neuskuteční z nějakého jiného důvodu, bude spojení restartováno v intervalu deseti sekund.

## Otevřené otázky
### Ukládání dat v konfiguračním souboru
Zatím nebylo zcela vymyšleno, v jakém formátu budou ukládána data v konfiguračním souboru.