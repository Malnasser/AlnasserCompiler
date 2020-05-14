Proc firstclass
_bp-8 = 5
_bp-2 = _bp-8
_bp-10 = 10
_bp-4 = _bp-10
Push _bp-4
Push _bp-2
Call secondclass
_bp-6 = _AX
wrs _S0
wri _bp-6
wrln
EndP firstclass
Proc secondclass
_bp-8 = _bp+4 * _bp+6
_bp-2 = _bp-8
_AX = _bp-2
EndP secondclass
Proc main
Call firstclass
Endp main
