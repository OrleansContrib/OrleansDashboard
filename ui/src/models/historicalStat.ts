export interface HistoricalStat {
  activationCount: number;
  recentlyUsedActivationCount: number;
  cpuUsage?: any;
  availableMemory?: any;
  memoryUsage: number;
  totalPhysicalMemory?: any;
  isOverloaded: boolean;
  clientCount: number;
  receivedMessages: number;
  sentMessages: number;
  dateTime: Date;
  receiveQueueLength?: number;
  requestQueueLength?: number;
  sendQueueLength?: number;
}

