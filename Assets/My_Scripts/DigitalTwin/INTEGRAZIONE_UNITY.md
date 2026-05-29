# Digital Twin VR Cagliari — Guida Integrazione Unity

## 1. Installa NativeWebSocket

`Window → Package Manager → + → Add package from git URL`

```
https://github.com/endel/NativeWebSocket.git#upm
```

> **Verifica**: in `manifest.json` comparirà `"com.endel.nativewebsocket": "..."`.

---

## 2. Copia gli script nel progetto

Copia tutta la cartella `DigitalTwin/` in:
```
Assets/My_Scripts/DigitalTwin/
```

File inclusi:
| File | Descrizione |
|---|---|
| `DTDataModels.cs` | Classi JSON (payload server) |
| `DTConfig.cs` | ScriptableObject configurazione |
| `DTWebSocketClient.cs` | Core WebSocket (Singleton) |
| `DTGuestController.cs` | Logica Meta Quest 3 |
| `DTDSSController.cs` | Logica PC Dashboard |
| `DTSemaforoVisual.cs` | Semaforo 3D visuale |

---

## 3. Crea l'asset DTConfig

```
Assets → Create → DigitalTwin → DTConfig
```

Imposta nel Inspector:
| Campo | Valore |
|---|---|
| Server Host | `localhost` (stesso PC) |
| Server Port | `8765` |
| Id Visore | MAC del Meta Quest 3 (es. `AA:BB:CC:DD:EE:FF`) |
| Reconnect Delay | `3` |

---

## 4. Setup scena VR Guest (GAP1.unity / GAP2.unity)

### 4a. DigitalTwinManager (nuovo GameObject)

```
GameObject → Create Empty → rinomina "DigitalTwinManager"
```

Aggiungi componenti:
- `DTWebSocketClient` → assegna DTConfig, scegli **VR Guest**
- `DTGuestController` → assegna DTConfig

### 4b. Semaforo 3D (GameObject esistente)

Sul GameObject 3D del semaforo nella scena:
- Aggiungi `DTSemaforoVisual`
- Assegna il `MeshRenderer` della luce
- Assegna eventuale `TextMeshPro` per il testo stato

Poi nel `DTGuestController`, assegna il `DTSemaforoVisual` nel campo `Semaforo Visual`.

### 4c. Materiale semaforo (URP)

Assicurati che il materiale del semaforo abbia **Emission** abilitata:
```
Material Inspector → Emission → spunta "Emission"
```

---

## 5. Setup scena VR DSS (Dashboard PC)

### 5a. DigitalTwinManager

Come sopra, ma `DTWebSocketClient` → **VR DSS**

### 5b. DTDSSController

Sul pannello UI principale:
- Aggiungi `DTDSSController`
- Assegna i campi TextMeshPro e Slider nell'Inspector

**Slider Soglie:**
- `SliderSogliaIntermedia`: Min=1, Max=100, Value=5
- `SliderSogliaLimite`: Min=1, Max=100, Value=10

**Pulsanti:**
- `BtnInviaSoglie.onClick` → `DTDSSController.InviaSoglie()`
- (opzionale) Pulsante Aggiorna → `DTDSSController.PollMetriche()`

---

## 6. Collegare a VoiceScript (già nel progetto)

Nel `VoiceScript.cs` esistente puoi integrare comandi vocali per le soglie:

```csharp
// Esempio integrazione VoiceScript → DTDSSController
DTDSSController dssController = FindObjectOfType<DTDSSController>();

if (comando.Contains("soglia emergenza"))
{
    dssController.ImpostaSoglie(intermedia: 20, limite: 50);
}
```

---

## 7. Ordine di avvio

```
1. mosquitto -v                    (Broker MQTT)
2. python server.py                (Core Twin + WebSocket)
3. python raspberry.py             (Edge)
4. python semaforo.py              (Attuatore)
5. python gateway.py               (Aggregatore)
6. python echobean.py              (Sensore)
7. python bracciale.py             (Generatore eventi)
8. Play in Unity Editor            (VR Guest o DSS)
```

Oppure per i nodi Python:
```bash
python launcher.py
```

---

## 8. Coordinate — nota importante

Il sistema Unity usa **coordinate UTM** (`Utm_east`, `Utm_north` in `GapData.cs`),
mentre il server Digital Twin usa **GeoJSON** `[Longitudine, Latitudine]`.

Per il sistema DT, le coordinate sono usate solo per identificare il varco fisico
nella twin logic — Unity usa il proprio sistema di coordinate di scena per il
posizionamento 3D. Non è necessario convertire le coordinate per il funzionamento
della comunicazione WebSocket.

---

## 9. Test rapido in Editor

Nel `DTWebSocketClient.cs`, verifica il log Unity:

```
[DT] Connessione a ws://localhost:8765 come [VRGuest]...
[DT] Connesso a ws://localhost:8765
[DT] Registrazione inviata: tipo=vr_guest
[DT Guest] Evento VR inviato: INGRESSO | visore=AA:BB:CC:DD:EE:FF
[DT] ← stato_varco=ok
[DT Guest] stato_varco aggiornato: ok
```
