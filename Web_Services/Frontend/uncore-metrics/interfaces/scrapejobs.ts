export interface ScrapeJobResponse {
    data?:  ScrapeJob[];
    error?: Error;
}

export interface ScrapeJob {
    name:          string;
    gameType:      string;
    runType:       string;
    node:          string;
    internalId:    string;
    runId:         number;
    progress:      number;
    totalDone:     number;
    totalCount:    number;
    runGuid:       string;
    running:       boolean;
    startedAt:     Date;
    lastUpdateUtc: Date;
}

export interface Error {
    code:    number;
    Message: string;
    type:    string;
}
