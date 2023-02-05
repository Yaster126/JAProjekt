; Konwerter plik�w WAVE stereo na mono
; Algorytm w j�zyku asemblera
; 29.01.2023, sem. 5, Grzegorz Nowak

; RCX - wska�nik na tablic� danych wej�ciowych (1 arg), RDX - rozmiar tablicy wej�ciowej (2 arg), R8 - wska�nik na tablic� danych wyj�ciowych (3 arg)

.data
half DWORD 0.5

.code
StereoToMonoAsm proc

	MOV R9, RDX								;ZAPIS ROZMIARU TABLICY
	SHR R9, 1								;ROZMIAR WYJSCIOWEJ
	XOR R10, R10							;POZYCJA WSKA�NIKA wej
	XOR R11, R11							;STRA�NIK WYPE�NIENIA YMM
	XOR R12, R12							;pozycja wska�nika wyj

	VBROADCASTSS YMM9, half					;wype�nienie YMM9 przez 0.5f

wpisz:
	VXORPD  ymm0, ymm0, ymm0				;wyzeruj YMM0
	VXORPD  ymm1, ymm1, ymm1				;wyzeruj YMM1
	VXORPD  ymm2, ymm2, ymm2				;Wyzeruj YMM2

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;wpisz 1 float do wektora
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;wpisz 2 float do wektora
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz
	
	SHUFPS XMM0, XMM0, 93H					;rotacje zawarto�ci wektor�w
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;wpisz 3 float do wektora
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;wpisz 4 float do wektora
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz
	
	SHUFPS XMM0, XMM0, 93H					;rotacje zawarto�ci wektor�w
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;5 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;6 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz

	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;7 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;8 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz									
													
	VPERM2F128 ymm0, ymm0, ymm0, 1H			;SWAP
	VPERM2F128 ymm1, ymm1, ymm1, 1H			; arg1 - rejestr docelowy, arg2 i 3 - rejestry do przestawienia, arg4 - instrukcja jak przestawi�

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;9 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;10 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz
	
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;11 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;12 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz
	
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;13 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;14 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX
	JZ dodaj								;je�li wpisano do wektor�w wszystkie przekazane liczby - skocz

	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;15 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]		;16 float
	INC R10
	INC R11

	DEC RDX
	DEC RDX

dodaj:
	VADDPS ymm8, ymm0, ymm1					;dodaj dwa wektory
	VMULPS ymm2, ymm8, ymm9					;pomn� dwa wektory

wypisz:
	CMP R11, 4
	JBE cztery								;je�li wpisanych liczb by�o 4 lub mniej (mie�ci si� w XMM) - skocz
	VPERM2F128 ymm2, ymm2, ymm2, 1H			;SWAP

	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;1 float

	INC R12
	DEC R9

	DEC R11

	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;2 float

	INC R12
	DEC R9

	DEC R11

	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;3 float

	INC R12
	DEC R9

	DEC R11

	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;4 float

	INC R12
	DEC R9

	DEC R11

	VPERM2F128 ymm2, ymm2, ymm2, 1H			;SWAP


cztery:
	SHUFPS XMM2, XMM2, 93H
	CMP R9, 3
	JE trzy									;je�li pozosta�y 3 liczby w wektorze - skocz
	SHUFPS XMM2, XMM2, 93H
	CMP R9, 2
	JE dwa									;je�li pozosta�y 2 liczby w wektorze - skocz
	SHUFPS XMM2, XMM2, 93H
	CMP R9, 1
	JE jeden								;je�li pozosta�a jedna liczba w wektorze - skocz

	SHUFPS XMM2, XMM2, 4EH					;przywr�cenie odpowiedniego ustawienia dla 4 liczb

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;5 float

	INC R12
	DEC R9

	DEC R11

trzy:
	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;6 float

	INC R12
	DEC R9

	DEC R11

dwa:
	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;7 float

	INC R12
	DEC R9

	DEC R11

jeden:
	SHUFPS XMM2, XMM2, 93H

	MOVSS DWORD PTR [R8+(R12*4)], XMM2		;8 float

	INC R12
	DEC R9
	JZ koniec

	DEC R11
	JZ WPISZ

	;SHUFPS XMM2, XMM2, 93H


koniec:
	ret
StereoToMonoAsm endp
end