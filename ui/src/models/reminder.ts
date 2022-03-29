export interface IReminder {
  period: string
  grainReference: string
  primaryKey: string
  activationCount: number
  name: string
  startAt: string
}

export interface ReminderData {
  reminders: IReminder[]
  count: number
}