import requests
import json
import os
import threading
import time

API_KEY = os.environ.get('MATCHMAKING_API_KEY')

def queue_player(i):

    # host = "https://robloxmatchmaking-bufqg4g2czczgpbb.canadacentral-01.azurewebsites.net"
    host = "http://localhost:5281"

    payload = {
        "PlayerId": i,
        "PartySize": 2,
        "PreferredRegion": "na",
        "GameMode": "tdm-2",
        "AccessCode": f"access_code_{i}"
    }

    headers = { 'content-type': "application/json", "x-api-key": API_KEY }

    print("sending request", i)
    response = requests.post(f"{host}/queue/players", json=payload, headers=headers)
    print(response.status_code)
    if (response.status_code != 200):
        print(response.content.decode())

threads = []
for i in range(0, 500):
    thread = threading.Thread(target=lambda: queue_player(i), daemon=True)
    threads.append(thread)
    thread.start()
    time.sleep(0.01)

for i in threads:
    i.join()
