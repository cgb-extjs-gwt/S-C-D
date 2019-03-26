import { List, Grid } from "@extjs/ext-react";

export class ExtDataviewHelper {

    public static getListSelected<T>(list: List, field?: string): T[] {
        if (field) {
            return this.getSelectedField(list, field);
        }
        else {
            return this.getSelected(list);
        }
    }

    public static getGridSelected<T>(grid: Grid, field?: string): T[] {
        if (field) {
            return this.getSelectedField(grid, field);
        }
        else {
            return this.getSelected(grid);
        }
    }

    public static refreshToolbar(grid: Grid & any) {
        grid.getPlugin('gridpagingtoolbar').setCurrentPage(1);
    }

    private static getSelected<T>(view: any): T[] {
        let selected: any[] = view.getSelections();
        let result: T[] = [];

        for (let i = 0, len = selected.length; i < len; i++) {
            result[i] = selected[i].data as T;
        }

        return result;
    }

    private static getSelectedField<T>(view: any, field: string): T[] {
        let selected: any[] = view.getSelections();
        let result: T[] = [];

        for (let i = 0, len = selected.length; i < len; i++) {
            result[i] = selected[i].data[field] as T;
        }

        return result;
    }
}