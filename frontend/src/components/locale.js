import Image from 'next/image';

export default function Locale({locale}) {
    let localeCountry = locale.split('-')[1];
    let localeUrl = `http://purecatamphetamine.github.io/country-flag-icons/3x2/${localeCountry}.svg`
    return (
        <>
            <Image src={localeUrl} width={30} height={22}/>
        </>
    );
}