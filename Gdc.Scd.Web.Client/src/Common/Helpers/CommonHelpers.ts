export const mapIf = <T>(
    items: T[], 
    conditionFn: ((item: T) => boolean), 
    mapFn: ((item: T) => T)) => 
    items.map(
        item => conditionFn(item) ? mapFn(item) : item)