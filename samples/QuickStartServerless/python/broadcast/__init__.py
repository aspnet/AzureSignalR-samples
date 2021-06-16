import requests
import json

import azure.functions as func


def main(myTimer: func.TimerRequest, signalRMessages: func.Out[str]) -> None:
    headers = {'User-Agent': 'serverless'}
    res = requests.get('https://api.github.com/repos/azure/azure-signalr', headers=headers)
    jres = res.json()

    signalRMessages.set(json.dumps({
        'target': 'newMessage',
        'arguments': [ 'Current star count of https://github.com/Azure/azure-signalr is: ' + str(jres['stargazers_count']) ]
    }))
