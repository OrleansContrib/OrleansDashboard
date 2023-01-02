async function wait(timeout: number) {
  return new Promise(resolve => {
    setTimeout(resolve, timeout);
  })
}

export default function setIntervalDebounced(action: () => void, interval: number) {
  let keepRunning = true

  const f = async () => {
    while (keepRunning) {
      try {
        action()
      }
      finally {
        await wait(interval)
      }
    }
  }
  f()

  return () => {
    keepRunning = false
  }

}