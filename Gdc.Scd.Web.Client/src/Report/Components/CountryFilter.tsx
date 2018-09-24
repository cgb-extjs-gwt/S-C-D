import { DictFilter, DictFilterProps } from "./DictFilter";

export class CountryFilter extends DictFilter {
    public constructor(props: DictFilterProps) {
        super({
            ...props,
            getItems: x => x.getCountries()
        });
    }
}