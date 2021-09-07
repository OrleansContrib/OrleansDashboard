import { DashboardCounters } from "../models/dashboardCounters";
import { get } from "./http";

export const getDashboardCounters = () => get<DashboardCounters>('DashboardCounters')

