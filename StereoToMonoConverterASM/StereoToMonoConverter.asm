; RCX - wskaŸnik na tablicê (1 arg), RDX - rozmiar tablicy (2 arg), R8 - 3 arg (wskaŸnik na tab wyj) (kana³y? Chyba olejê, bo nie wiem jak dynamicznie dobieraæ rejestry...)

.data
half DWORD 0.5
one DWORD 1
;TEMP DWORD

.code
StereoToMonoAsm proc

	MOV R12, RDX								;ZAPIS ROZMIARU TABLICY
	MOV R9, R12
	SHR R9, 1								;ROZMIAR WYJSCIOWEJ
	XOR R10, R10							;POZYCJA WSKANIKA
	XOR R11, R11							;STRA¯NIK WYPE£NIENIA YMM
	VZEROALL

	VBROADCASTSS YMM9, half
	;VBROADCASTSS YMM9, one

wpisz:

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;1 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX

	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;2 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX

	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;3 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX

	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;4 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX
	JZ dodaj

	CMP R11, 1										; To ca³e mo¿e wylecieæ....
	JE dodaj										;
													;
	VPERM2F128 ymm0, ymm0, ymm0, 1H					
	VPERM2F128 ymm1, ymm1, ymm1, 1H					
													;
	INC R11											;

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;5 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX
	
	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;6 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX
	
	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;7 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX
	
	JZ dodaj
	SHUFPS XMM0, XMM0, 93H
	SHUFPS XMM1, XMM1, 93H

	ADDSS XMM0, DWORD PTR [RCX+(R10*4)]		;8 float
	INC R10
	ADDSS XMM1, DWORD PTR [RCX+(R10*4)]
	INC R10

	DEC RDX
	DEC RDX
	JZ dodaj
	
	

dodaj:
	VADDPS ymm8, ymm0, ymm1
	VMULPS ymm2, ymm8, ymm9
	;VPSRLD ymm2, ymm8, ymm9				;AVX2 NIE DZIA£A MIMO ¯E POWINNO!! >_<
	XOR R11, R11

wypisz:
	DEC R9
	MOVSS DWORD PTR [R8+(R9*4)], XMM2
	;movss rax, xmm2
	PEXTRD EAX, XMM2, 0
	;MOV [RCX+(R9*4)], EAX


koniec:
ret
StereoToMonoAsm endp
end