import requests
import json

import azure.functions as func

etag = ''
start_count = 0

def main(myTimer: func.TimerRequest, signalRMessages: func.Out[str]) -> None:
    global etag
    global start_count
    headers = {'User-Agent': 'serverless', 'If-None-Match': etag}
    res = requests.get('https://api.github.com/repos/azure/azure-signalr', headers=headers)
    if res.headers.get('ETag'):
        etag = res.headers.get('ETag')

    if res.status_code == 200:
        jres = res.json()
        start_count = jres['stargazers_count']
    
    signalRMessages.set(json.dumps({
        'target': 'newMessage',
        'arguments': [ 'Current star count of https://github.com/Azure/azure-signalr is: ' + str(start_count) ]
    }))
