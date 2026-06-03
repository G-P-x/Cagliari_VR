## NODE: Bracciale %% Dispositivo Simulato in Python %%
**Nome Visivo**: Bracciale IoT  
**Layer**: Observable Element Layer %% ISO 23247 %%  
**Tipo**: Device  
**Descrizione**: Dispositivo di rilevamento dati personali e geolocalizzazione  

    Input 
        - ingresso %% Generato %%
		 
    Output (>EchoBean) %% Protocollo: Bluetooth Low Energy (BLE) %%
        - id bracciale %% mac address %%
        - ingresso 

## NODE: EchoBean %% Dispositivo Simulato in Python %%
**Nome Visivo**: Echo Bean IoT  
**Layer**: Observable Element Layer %% ISO 23247 %%  
**Tipo**: Device  
**Descrizione**: Sensore ambientale e rilevamento presenza 

    Input (<Bracciale)
        - id bracciale
        - ingresso
    Output (>Gateway) %% Protocollo: BLE / Zigbee %%
        - id bracciale %% mac address %%
        - id echobean %% mac address %%
        - coordinate varco %% Generato: Hardcoded/Config %%
        - ingresso

## NODE: Gateway %% Dispositivo Simulato in Python %%
**Nome Visivo**: Gateway di Rilevamento  
**Layer**: Device Communication Layer %% ISO 23247 %%  
**Tipo**: Process  
**Descrizione**: Punto di aggregazione dati da dispositivi IoT  

    Input (<EchoBean)
        - id bracciale
        - id echobean %% mac address %%
        - coordinate varco
        - ingresso
    Output (>Raspberry) %% Protocollo: MQTT / HTTP REST %%
        - id gateway
        - id bracciale
        - id echobean 
        - timestamp %% Generato: Real Time Clock interno %%
        - ingresso
        - coordinate varco

## NODE: Raspberry %% Dispositivo Simulato in Python %%
**Nome Visivo**: Controller di Controllo Accessi  
**Layer**: Digital Twin Entity Layer %% ISO 23247 - Edge Elaboration %%  
**Tipo**: Process  
**Descrizione**: Esegue la logica di decisione per il semaforo di controllo accessi. Client MQTT connesso al broker del Server

    Input (<Gateway)
        - id gateway
        - id bracciale
        - id echobean
        - timestamp
        - ingresso
        - coordinate varco
    Input (<Server)
	    - stato semaforo %% Generato: rosso, giallo, verde %%
    Output (>Server) %% Protocollo: MQTT / gRPC %%
        - id gateway
        - stato varco %% Generato: Errore di comunicazione EchoBean %%
        - nome varco %% Generato %%
        - ingresso
        - data %% Generato: NTP %%
        - timestamp %% Generato: NTP %%
        - coordinate varco
    Output (>Semaforo Led) %% Protocollo: MQTT / gRPC %%
	    - stato semaforo %% rosso, giallo, verde %%

## NODE: Semaforo Led %% Dispositivo Simulato in Python %%
**Nome Visivo**: Semaforo Controllo Accessi  
**Layer**: Observable Element Layer %% ISO 23247 - Livello Attuazione %%  
**Tipo**: Device  
**Descrizione**: Semplificatore stato ingresso/uscita server  
 
    Input (<Raspberry) %% Protocollo: GPIO / MQTT %%
        - stato semaforo %% rosso, giallo, verde %%
    Output
        - stato led %% rosso, giallo, verde %%

## NODE: Server %% Dispositivo Simulato in Python %%
**Nome Visivo**: Server gestione  
**Layer**: Digital Twin Entity Layer %% ISO 23247 - Core Twin Logic & DB %%  
**Tipo**: Process
**Descrizione**: Middleware di elaborazione e database. Ospita il Broker MQTT (Mosquitto) per lo smistamento dei messaggi IoT 

    Input (<Raspberry) %% DATI DA GENERARE CON PYTHON %% 
        - id gateway
        - stato varco
        - nome varco
        - ingresso
        - data
        - timestamp
        - coordinate varco
    Output (>Raspberry %% Protocollo: GPIO / MQTT %%)
        - stato semaforo %% Generato: rosso, giallo, verde %%
		  
    Input (<VR Guest) %% Protocollo: WebSockets / TCP %%
        - ingresso vr %% per ogni ingresso in scena VR %%
        - uscita vr %% per ogni uscita scena %%
        - datetimestamp vr %% per ogni ingresso in scena VR %%
        - id visore %% mac address visore %%
    Output (>VR Guest)
        - stato varco %% varco funzionante o no %%
		  
    Input (<VR DSS) %% Protocollo: WebSockets / TCP %%
        - soglia ingressi intermedia %% stato led giallo per varco %%
        - soglia ingressi limite %% stato led rosso per varco %%
        - data start
        - data end
    Output (>VR DSS)
        - tipo %% definisce il tipo di informazione, Generato %%
        - stato varco
        - visitatori %% Generato %%
        - visitatori vr %% Generato %%
        - errore standard %% Generato %%
        - media %% Generato %%

## NODE: VR Guest %% Visore Meta Quest 3 %%
**Nome Visivo**: Guest Application VR  
**Layer**: User Application Layer %% ISO 23247 %%  
**Tipo**: Process  
**Descrizione**: Interfaccia utente VR per visitatori

    Input (<Server)
        - stato varco %% varco funzionante o no %%
    Output (>Server)
        - ingresso vr %% per ogni ingresso in scena VR %%
        - uscita vr %% per ogni uscita scena %%
        - datetimestamp vr %% per ogni ingresso in scena VR %%
        - id visore %% mac address visore %%

## NODE: VR DSS %% VR su PC %%
**Nome Visivo**: Dashboard DSS VR  
**Layer**: User Application Layer %% ISO 23247 %%  
**Tipo**: Process  
**Descrizione**: Interfaccia utente VR per decisore 

    Input (<Server)
        - tipo
        - stato varco
        - visitatori
        - visitatori vr
        - errore standard
        - media
    Output (>Server)
        - soglia ingressi intermedia %% stato led giallo per varco %%
        - soglia ingressi limite %% stato led rosso per varco %%
        - data start
        - data end