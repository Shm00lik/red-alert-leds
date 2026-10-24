# red-alert-leds

An LED strip that shows Israeli Home Front Command alerts for the cities you care about.

```
Tzofar websocket ──► redalert server (.NET, Docker) ──MQTT──► Mosquitto (Docker) ──MQTT──► ESP8266 + LED strip
                            ▲
                     control page (React)
```

- **Server** (`RedAlertLEDs/RedAlertLEDs`): listens to the Tzofar (tzevaadom) websocket, filters alerts by your cities and publishes one LED command per state change on `leds/cmd`. It also serves the control page and its API.
- **Control page** (`RedAlertLEDs/web`): pick cities, see the live state and countdown, set state colors and brightness, drive the LEDs by hand, and see recent alerts.
- **Broker**: a shared Mosquitto stack (separate Docker compose on the home server, network `mqtt`).
- **ESP firmware**: a separate project. It subscribes to `leds/cmd` (JSON effects) and `leds/frame` (raw RGB).

## What the strip shows

| Event | Strip |
|---|---|
| Early warning | early-warning color, draining over 10 min |
| Missiles / hostile aircraft | alert color, draining over 10 min |
| Incident ended | all-clear color, draining over 1 min, then off |
| Server down | off (MQTT last will) |

No cities selected means all of Israel.

## Run on the home server

Prerequisite: the Mosquitto stack is running and the Docker network `mqtt` exists.

```bash
cd ~/projects
git clone <this repo> red-alert-leds
cd red-alert-leds/deploy
cp .env.example .env                       # fill in Mqtt__Password from ~/projects/mosquitto/credentials.txt
mkdir -p data && sudo chown 1654 data      # the container runs as UID 1654
docker compose up -d --build
```

Open `http://<server>:8080`.

## Local development

```bash
cd RedAlertLEDs/RedAlertLEDs
Mqtt__Host=<broker> Mqtt__Username=redalert Mqtt__Password=... dotnet run   # http://localhost:8080
```

```bash
cd RedAlertLEDs/web
npm install
npm run dev                                   # http://localhost:5173, proxies /api to :8080
```

```bash
cd RedAlertLEDs
dotnet test
```

Settings (`settings.json`) and the cached city list (`cities.json`) are stored in `DATA_DIR` (default `./data`).

## API

| Method | Path | |
|---|---|---|
| GET | `/api/status` | state, countdown, Tzofar/MQTT/ESP connection |
| GET | `/api/cities?q=` | search cities (Hebrew or English) |
| GET / PUT | `/api/settings` | cities, state colors, brightness |
| POST | `/api/led` | manual LED command, e.g. `{"effect":"rainbow"}` |
| POST | `/api/led/state/{Idle\|EarlyWarning\|Alert\|Safe}` | simulate a state |
| GET | `/api/alerts` | last 20 relevant alerts |
