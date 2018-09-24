import { DictFilter, DictFilterProps } from "./DictFilter";

export class YearFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getYears()
        });
    }
}