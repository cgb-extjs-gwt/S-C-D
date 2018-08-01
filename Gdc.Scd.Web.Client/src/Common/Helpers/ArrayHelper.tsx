export class ArrayHelper {

    public static firstOrDefault<T>(items: T[]): T {
        return items && items.length > 0 ? items[0] : null;
    }

}