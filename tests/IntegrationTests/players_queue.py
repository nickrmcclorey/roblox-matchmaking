import http.client
import json
import os
import threading

start = 1000

def queue_player(i):
    conn = http.client.HTTPConnection("localhost:5281")

    payload = "{\n  \"PlayerId\": 1,\n  \"PartySize\": 2,\n  \"PreferredRegion\": \"na\",\n  \"GameMode\": \"ctf-2\"\n}"
    payload = {
        "PlayerId": i,
        "PartySize": 2,
        "PreferredRegion": "na",
        "GameMode": "ctf-2",
        "AccessCode": f"access_code_{i}"
    }

    headers = { 'content-type': "application/json", "x-api-key": os.environ.get('MATCHMAKING_API_KEY') }

    conn.request("POST", "/queue/players", json.dumps(payload), headers)
    print("sent request", i)
    res = conn.getresponse()
    data = res.read()

    print(res.getcode())
    if res.getcode() != 200:
        print(data.decode("utf-8"))

threads = []
for i in range(1, 5):
    thread = threading.Thread(target=lambda: queue_player(i), daemon=True)
    threads.append(thread)
    thread.start()
    
for i in threads:
    i.join()
