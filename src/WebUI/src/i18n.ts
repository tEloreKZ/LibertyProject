export type Translate = (key: string, ...args: Array<string | number>) => string

export function createTranslator(strings: Record<string, string>): Translate {
  return (key, ...args) => {
    let value = strings[key] ?? key
    args.forEach((argument, index) => {
      value = value.replaceAll(`{${index}}`, String(argument))
    })
    return value
  }
}
