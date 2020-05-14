.model small
.stack 100h
.data
_S0 db "c = ","$"
.code
include io.asm
start PROC
mov ax, @data
mov ds, ax
call main
mov ah, 04ch
int 21h
start ENDP
firstclass Proc
push bp
mov bp, sp
sub sp, 10
mov ax, 5
mov [bp-8], ax
mov ax, [bp-8]
mov [bp-2], ax
mov ax, 10
mov [bp-10], ax
mov ax, [bp-10]
mov [bp-4], ax
push [bp-4]
push [bp-2]
call secondclass
mov [bp-6], ax
mov dx, OFFSET _S0
call writestr
mov ax, [bp-6]
call writeint
call writeln
add sp, 10
pop bp
ret 0
firstclass EndP
secondclass Proc
push bp
mov bp, sp
sub sp, 8
mov ax, [bp+4]
mov bx, [bp+6]
imul bx
mov [bp-8], ax
mov ax, [bp-8]
mov [bp-2], ax
mov ax, [bp-2]
add sp, 8
pop bp
ret 4
secondclass EndP
main Proc
call firstclass
ret
main Endp
END start
