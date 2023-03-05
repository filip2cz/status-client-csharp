# Funkční specifikace

- verze: 0.1
- datum: 5. března 2023
- autor: Filip Komárek

## O tomto dokumentu
Účel tohoto dokumentu je specifikovat funkční požadavky.  
Dokument je určen pro uživatele, kteří chtějí lépe pochopit fungování tohoto programu.

## Použití
Software bude sloužit jako klient k softwaru Status od BotoX (https://github.com/BotoX/ServerStatus). Software bude neustále spuštěn na zařízení a bude pravidelně každou vteřinu odesílat data serveru, který je pak bude zobrazovat na webové stránce (příklad serveru: https://status.fkomarek.eu). Software budou pouze odesílat data serveru, nebude nijak zobrazovat uživateli cokoliv.

## Architektura software
Po spuštění si software ze souboru získá uživatele a heslo pro autentizaci a poté je odešle serveru pro ověření identity, jehož adresu také získá z konfiguračního souboru. Odesílaní data jsou ve formátu `uživatel:heslo`  
Poté bude každou vteřinu získávat data o aktuálním stavu systému a odesílat je serveru. Odesílaná data jsou ve formátu: `update {\"online6\": false,  \"uptime\": 229, \"load\": 0.93, \"memory_total\": 12200460, \"memory_used\": 2499944, \"swap_total\": 12201980, \"swap_used\": 0, \"hdd_total\": 3761418, \"hdd_used\": 1161319, \"cpu\": 19.0, \"network_rx\": 0, \"network_tx\": 0 }`  
