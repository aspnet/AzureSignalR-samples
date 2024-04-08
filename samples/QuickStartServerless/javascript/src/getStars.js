const fetch = require('node-fetch');

const URL = "https://api.github.com/repos/azure/azure-signalr"

async function getStars(currentEtag){

    const headers = {
        'If-None-Match': currentEtag
    };

    const response = await fetch(URL, { headers });
    if(response.ok){

        const etag = response.headers.get('etag');
        const { stargazers_count } = await response.json();

        return { etag, stars: stargazers_count };
    }
    throw new Error('Failed to fetch data: ' + response.status + ' ' + response.statusText);
}

module.exports = getStars;