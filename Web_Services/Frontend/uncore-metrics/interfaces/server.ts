export interface SingleServerResponse {
    data?:  Server;
    error?: Error;
}


export interface Server {
    serverID:       string;
    name:           string;
    game:           string;
    map:            string;
    appID:          number;
    ipAddressBytes: string;
    ipAddress:      string;
    port:           number;
    queryPort:      number;
    players:        number;
    maxPlayers:     number;
    retriesUsed:    number;
    asn:            number;
    isp:            string;
    latitude:       number;
    longitude:      number;
    country:        string;
    continent:      number;
    timezone:       string;
    isOnline:       boolean;
    serverDead:     boolean;
    lastCheck:      Date;
    nextCheck:      Date;
    failedChecks:   number;
    foundAt:        Date;
}

export interface Error {
    code:    number;
    Message: string;
    type:    string;
}
