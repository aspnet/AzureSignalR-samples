import os

import azure.functions as func


def main(req: func.HttpRequest) -> func.HttpResponse:
    f = open(os.path.dirname(os.path.realpath(__file__)) + '/../content/index.html')
    return func.HttpResponse(f.read(), mimetype='text/html')
