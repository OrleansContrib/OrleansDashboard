
export interface Call {
  grain: string;
  method: string;
  count: number;
  exceptionCount: number;
  elapsedTime: number;
  numberOfSamples: number;
}

export interface Latency {
  grain: string;
  method: string;
  count: number;
  exceptionCount: number;
  elapsedTime: number;
  numberOfSamples: number;
}

export interface Error {
  grain: string;
  method: string;
  count: number;
  exceptionCount: number;
  elapsedTime: number;
  numberOfSamples: number;
}

export interface TopGrainMethods {
  calls: Call[];
  latency: Latency[];
  errors: Error[];
}

