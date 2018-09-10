export function numOrEmpty(v: number): any {
    return typeof v === 'number' ? v : ' ';
}