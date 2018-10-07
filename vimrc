set rtp+=~/.vim/bundle/Vundle.vim
call vundle#rc()
Plugin 'othree/vim-autocomplpop'

set ruler
set showmode
set background=dark
set hlsearch
set cursorline
set number
 
set expandtab
set shiftwidth=4
set tabstop=4
set softtabstop=4
set backspace=2

set autoindent
set smartindent

set fileencodings=utf8,big5

nmap <bslash>p :set paste!<CR>
nmap <bslash>x mzHmx:silent! :%s/[ \t][ \t]*$//g<CR>`xzt`z
nmap <bslash>t :Tlist<CR>

nmap <F9> <ESC>\x:w<CR>:!g++ -O2 -std=c++11 % -o %<<CR>
nmap <F12> <ESC>\x:w<CR>:!./%<<CR>

imap <F9> <ESC>\x:w<CR>:!g++ -O2 -std=c++11 % -o %<<CR>
imap <F12> <ESC>\x:w<CR>:!./%<<CR>

nmap <F3> :tabp<ENTER>
nmap <F4> :tabn<ENTER>

syntax on

