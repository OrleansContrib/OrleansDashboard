
export interface Host {
  faultZone: number;
  hostName: string;
  iAmAliveTime: Date;
  proxyPort: number;
  roleName: string;
  siloAddress: string;
  siloName: string;
  startTime: Date;
  status: string;
  updateZone: number;
  siloStatus: number;
}

export interface SimpleGrainStat {
  activationCount: number;
  grainType: string;
  siloAddress: string;
  totalAwaitTime: number;
  totalCalls: number;
  callsPerSecond: number;
  totalSeconds: number;
  totalExceptions: number;
}

export interface DashboardCounters {
  hosts: Host[];
  simpleGrainStats: SimpleGrainStat[];
  totalActiveHostCount: number;
  totalActiveHostCountHistory: number[];
  totalActivationCount: number;
  totalActivationCountHistory: number[];
}

