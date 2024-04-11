const fetch = require('node-fetch');

const URL = "https://api.github.com/repos/azure/azure-signalr"

async function getStars(currentEtag){

    console.log(`currentEtag: ${currentEtag}`);
    const headers = {
        'If-None-Match': currentEtag
    };

    const response = await fetch(URL, { headers });
    if(response.ok){

        const etag = response.headers.get('etag');
        const { stargazers_count } = await response.json();

        console.log(`Current star count is: ${stargazers_count}`);

        return { etag, stars: stargazers_count };
    } else {
        console.log('Failed to fetch data: ' + response.status + ' ' + response.statusText);
        return { etag: currentEtag, stars: undefined };
    }
}

module.exports = getStars;