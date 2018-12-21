export const mapIf = <T>(
    items: T[], 
    conditionFn: ((item: T) => boolean), 
    mapFn: ((item: T) => T)) => 
        items.map(
            item => conditionFn(item) ? mapFn(item) : item)

export const objectPropsEqual = <T extends {[key: string]: any }>(obj1: T, obj2: T, ...props: string[]) => {
    let result = true;

    if (obj1 != obj2) {
        if (!obj1 || !obj2) {
            result = false;
        } else {
            if (props.length == 0){
                props = Object.keys(obj1);
            } 

            result = props.every(key => obj1[key] == obj2[key]);
        }
    }

    return result
}