
// Generated by https://quicktype.io

export interface LocationRequest {
    data: PingLocation[];
}

export interface PingLocation {
    locationID:   number;
    locationName: string;
    isp:          string;
    asn:          string;
    latitude:     number;
    longitude:    number;
    country:      string;
}
