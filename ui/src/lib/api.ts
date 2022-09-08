import { Stats } from '../models/stats'
import { DashboardCounters } from '../models/dashboardCounters'
import { HistoricalStat } from '../models/historicalStat'
import { TopGrainMethods } from '../models/topGrainMethods'
import { get } from './http'
import { Properties } from '../models/properties'
import { ReminderData } from '../models/reminder'

export const getDashboardCounters = () =>
  get<DashboardCounters>('DashboardCounters')

export const getClusterStats = () => get<Stats>('ClusterStats')

export const getTopGrainMethods = () => get<TopGrainMethods>('TopGrainMethods')

export const getGrainStats = (grainType: string) =>
  get<Stats>(`GrainStats/${grainType}`)

export const getHistoricalStats = (host: string) =>
  get<HistoricalStat[]>(`HistoricalStats/${host}`)

export const getSiloStats = (host: string) => get<Stats>(`SiloStats/${host}`)

export const getSiloProperties = (host: string) =>
  get<Properties>(`SiloProperties/${host}`)

export const getReminders = (page: number) =>
  get<ReminderData>(`Reminders/${page}`)

export const getGrainState = (type: string, id: string) =>
  get<{value:string}>(`GrainState?grainType=${type}&grainId=${id}`)

export const getGrainTypes = () => get<{ value: string[] }>('GrainTypes')
