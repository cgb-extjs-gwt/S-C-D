export const toArray = <T=any>(value: any): T[] => {
    return Array.isArray(value) ? value : [value];
}