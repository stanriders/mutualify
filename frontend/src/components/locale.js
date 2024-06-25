import Image from 'next/image';

export default function Locale({locale}) {
    let localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/NP.svg'
    switch(locale) {
        case 'en-US':
            localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/US.svg'; break;
        case 'ru-RU':
            localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/RU.svg'; break;
        case 'fi-FI':
            localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/FI.svg'; break;
        case 'pl-PL':
            localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/PL.svg'; break;
        case 'sv-SE':
            localeUrl = 'http://purecatamphetamine.github.io/country-flag-icons/3x2/SE.svg'; break;
    }

    return (
        <>
            <Image src={localeUrl} width={30} height={22}/>
        </>
    );
}