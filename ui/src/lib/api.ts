import { ClusterStats } from "../models/clusterStats";
import { DashboardCounters } from "../models/dashboardCounters";
import { TopGrainMethods } from "../models/topGrainMethods";
import { get } from "./http";

export const getDashboardCounters = () => get<DashboardCounters>('DashboardCounters')

export const getClusterStats = () => get<ClusterStats>('ClusterStats')

export const getTopGrainMethods = () => get<TopGrainMethods>('TopGrainMethods')


