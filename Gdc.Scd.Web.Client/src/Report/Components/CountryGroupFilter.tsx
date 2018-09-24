import { DictFilter, DictFilterProps } from "./DictFilter";

export class CountryGroupFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getCountryGroups()
        });
    }
}